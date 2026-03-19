-- ============================================================
-- Table: fps.tblsettings
-- 
-- Redesigned application settings table.
--
-- BACKGROUND
-- ----------
-- The original tblsettings (SQL Server) held 14 key-value rows
-- mixing business-logic constants, MS Access front-end version
-- numbers, UNC upgrade-script paths, and infrastructure config.
-- Four additional MS Access local tables (PACT, FPS, CostBook,
-- PIMS) duplicated connection strings and run-mode flags.
--
-- DESIGN DECISIONS
-- ----------------
-- 1. Only business-logic settings that the application needs at
--    runtime belong in this table.
--
-- 2. Infrastructure / connection settings (SQLServer, SQLDatabase,
--    MABDatabase, SnapshotDatabase, Cos90Location, RunMode,
--    SystemName, MajorVersionNo) move to appsettings.json.
--
-- 3. Obsolete settings are dropped entirely:
--      • Ad6000_ID, Ad6000_WG  – disabled feature (frmMTConversion)
--      • RequiredFrontEndVersion_FPS / _Pact / _FPS_2013 / _Pact_2013
--      • UpgradeFile_FPS / _Pact / _FPS2013 / _Pact2013
--    These were MS Access front-end versioning controls and are not
--    relevant after modernisation.
--
-- CHANGES FROM ORIGINAL tblsettings
-- ----------------------------------
--   • Widened 'notes'      – changed from VARCHAR(255) to TEXT so full
--                            descriptions can be stored
--   • Added 'updated_by'   – audit trail: who last changed the row
--   • Added 'updated_at'   – audit trail: when the row was last changed
--   • Kept 'fpsyear'       – preserves fiscal-year scoping
--
-- COLUMNS REMOVED (with reasons)
-- --------------------------------
--   • 'testsetting' VARCHAR(255)
--       REASON: This column held an alternate setting value used when
--       RunMode was set to 'Test'. The application would read testsetting
--       instead of setting to allow testing with different values without
--       modifying production data. With modernisation, RunMode is moving
--       to appsettings.json and environment-specific configuration is
--       handled via separate environment files (appsettings.Development.json,
--       appsettings.Production.json) or environment variables. The
--       database no longer needs to carry test overrides.
--
--   • 'category' VARCHAR(50)  [was proposed, then removed]
--       REASON: Originally proposed to group settings by functional area
--       (General, Leave, Costbook). With only 5 business-logic settings
--       remaining in this table, categorisation adds no practical value.
--       Each setting is self-descriptive via its id and notes columns.
--       Adding a category column would be over-engineering for such a
--       small dataset.
--
--   • 'data_type' VARCHAR(20)  [was proposed, then removed]
--       REASON: Originally proposed as a type hint (string, decimal,
--       integer, path) to tell consumers how to parse the varchar value.
--       Removed because the consuming application code already knows the
--       expected type for each well-known setting key. For example,
--       HoursInDay is always parsed as decimal, LeaveEntitlement as
--       integer. A metadata column adds maintenance overhead without
--       value when the set of settings is small and stable.
-- ============================================================

CREATE TABLE fps.tblsettings (

    -- --------------------------------------------------------
    -- id: The unique setting key.
    -- Examples: 'HoursInDay', 'LeaveEntitlement'
    -- This is the natural primary key; application code
    -- references settings by this name.
    -- --------------------------------------------------------
    id              VARCHAR(50)     NOT NULL,

    -- --------------------------------------------------------
    -- setting: The setting value stored as text.
    -- e.g. '7.2', '30', '0.6'
    -- --------------------------------------------------------
    setting         VARCHAR(255),

    -- --------------------------------------------------------
    -- notes: Free-text description of the setting, its origin,
    -- and where it is used.  Changed to TEXT (was VARCHAR(255))
    -- because many descriptions exceed 255 characters.
    -- --------------------------------------------------------
    notes           TEXT,

    -- --------------------------------------------------------
    -- fpsyear: Fiscal / FPS year the setting applies to.
    -- NULL means the setting is not year-specific.
    -- --------------------------------------------------------
    fpsyear         INTEGER NOT NULL,

    -- --------------------------------------------------------
    -- updated_by: Username or service account that last
    -- modified this row.  Useful for audit / change tracking.
    -- --------------------------------------------------------
    updated_by      VARCHAR(100),

    -- --------------------------------------------------------
    -- updated_at: Timestamp of the last modification.
    -- Defaults to the current time on INSERT; should be
    -- refreshed on every UPDATE.
    -- --------------------------------------------------------
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),

    -- Primary key
    CONSTRAINT pk_tblsettings PRIMARY KEY (id, fpsyear),

    CONSTRAINT fk_tblsettings_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
