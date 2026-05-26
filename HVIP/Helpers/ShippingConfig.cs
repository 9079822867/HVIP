using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    /// <summary>
    /// Thread-safe cached shipping configuration.
    /// Reads once from the ShippingSettings table; call Reload() after admin saves.
    /// Falls back to ₹60 charge / ₹500 free threshold if the DB row is absent.
    /// </summary>
    public static class ShippingConfig
    {
        private static volatile bool _loaded = false;
        private static decimal _threshold = 500m;
        private static decimal _charge    = 60m;
        private static readonly object _lock = new object();

        // ── Public accessors ──────────────────────────────────

        public static decimal FreeThreshold
        {
            get { EnsureLoaded(); return _threshold; }
        }

        public static decimal Charge
        {
            get { EnsureLoaded(); return _charge; }
        }

        /// <summary>Returns the shipping cost for a given subtotal.</summary>
        public static decimal Calculate(decimal subTotal)
        {
            EnsureLoaded();
            return subTotal >= _threshold ? 0m : _charge;
        }

        /// <summary>Returns the current settings as a model (for the admin form).</summary>
        public static ShippingSettings GetSettings()
        {
            EnsureLoaded();
            return new ShippingSettings { FreeThreshold = _threshold, Charge = _charge };
        }

        /// <summary>Persists settings to DB and invalidates the cache.</summary>
        public static bool SaveSettings(decimal freeThreshold, decimal charge)
        {
            try
            {
                const string sql = @"
                    IF EXISTS(SELECT 1 FROM ShippingSettings WHERE Id=1)
                        UPDATE ShippingSettings
                           SET FreeThreshold=@T, Charge=@C, UpdatedOn=GETDATE()
                         WHERE Id=1
                    ELSE
                        INSERT INTO ShippingSettings(Id, FreeThreshold, Charge, UpdatedOn)
                        VALUES(1, @T, @C, GETDATE())";

                DbHelper.Execute(sql, new[]
                {
                    new SqlParameter("@T", freeThreshold),
                    new SqlParameter("@C", charge)
                });
                Reload();
                return true;
            }
            catch { return false; }
        }

        /// <summary>Forces a reload from DB on next access.</summary>
        public static void Reload()
        {
            lock (_lock) { _loaded = false; }
        }

        // ── Private helpers ───────────────────────────────────

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            lock (_lock)
            {
                if (_loaded) return;
                Load();
                _loaded = true;
            }
        }

        private static void Load()
        {
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(
                    "SELECT TOP 1 FreeThreshold, Charge FROM ShippingSettings WHERE Id=1", conn))
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        _threshold = (decimal)r["FreeThreshold"];
                        _charge    = (decimal)r["Charge"];
                        return;
                    }
                }
            }
            catch { }
            // table missing or no row → keep defaults
            _threshold = 500m;
            _charge    = 60m;
        }
    }
}
