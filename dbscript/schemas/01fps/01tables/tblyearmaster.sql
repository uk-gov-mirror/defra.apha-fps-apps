-- ============================================================
-- Table: fps.tblyearmaster
--
-- Master table of fiscal / FPS years.
--
-- BACKGROUND
-- ----------
-- The application operates on a fiscal-year basis (e.g.
-- FPS2024-25, FPS2025-26).  This table is the single source
-- of truth for which years exist, their human-readable codes,
-- and their lifecycle status.
--
-- Other tables that carry an fpsyear column (tblperiod,
-- proposedtblsettings, tblcurrentmonth, etc.) should
-- reference this table via a foreign key so that only valid
-- years can be used.
--
-- COLUMN NOTES
-- ------------
--   fpsyear      – The four-digit calendar year that starts
--                   the fiscal period (e.g. 2025 for Apr 2025
--                   to Mar 2026).  Natural primary key.
--   fpsyearcode  – Display code shown in the UI and reports
--                   (e.g. 'FPS2025-26').
--   yearstatus   – Lifecycle state: 'Open', 'Closed', or
--                   'Planned'.
--   remarks      – Free-text note explaining the status or
--                   any special conditions for the year.
--   active       – Soft-delete / visibility flag.
--                   TRUE = visible to the application.
--   createdon    – Timestamp when the row was first inserted.
--   createdby    – Username or service account that created
--                   the row.
-- ============================================================

CREATE TABLE fps.tblyearmaster (

    -- --------------------------------------------------------
    -- fpsyear: Four-digit calendar year that opens the fiscal
    -- period.  This is the natural primary key; every other
    -- table that stores a year value should FK back here.
    -- --------------------------------------------------------
    fpsyear         INTEGER         NOT NULL,

    -- --------------------------------------------------------
    -- fpsyearcode: Human-readable fiscal-year label shown in
    -- the UI and printed reports.
    -- Format: 'FPS<start>-<end two digits>'
    -- e.g. 'FPS2025-26'
    -- --------------------------------------------------------
    fpsyearcode     VARCHAR(20)     NOT NULL,

    -- --------------------------------------------------------
    -- yearstatus: Current lifecycle state of the fiscal year.
    --   'Open'    – active year; transactions are allowed.
    --   'Closed'  – historical year; read-only.
    --   'Planned' – future year; configuration only, no
    --               transactions yet.
    -- A CHECK constraint enforces the allowed values.
    -- --------------------------------------------------------
    yearstatus      VARCHAR(10)     NOT NULL,

    -- --------------------------------------------------------
    -- remarks: Free-text note providing context for the
    -- current status or any special conditions.
    -- --------------------------------------------------------
    remarks         TEXT,

    -- --------------------------------------------------------
    -- active: Soft-delete / visibility flag.
    --   TRUE  (default) – year is visible to the application.
    --   FALSE           – year is hidden / deactivated.
    -- --------------------------------------------------------
    active          BOOLEAN         NOT NULL DEFAULT TRUE,

    -- --------------------------------------------------------
    -- createdon: Timestamp of row creation.
    -- Automatically set to the current time on INSERT.
    -- --------------------------------------------------------
    createdon       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),

    -- --------------------------------------------------------
    -- createdby: The user or service account that created
    -- this row.  Should be populated by the application layer.
    -- --------------------------------------------------------
    createdby       VARCHAR(100),

    -- Primary key
    CONSTRAINT pk_tblyearmaster PRIMARY KEY (fpsyear),

    -- Ensure fpsyearcode is unique across all rows
    CONSTRAINT uq_tblyearmaster_fpsyearcode UNIQUE (fpsyearcode),

    -- Restrict yearstatus to known values
    CONSTRAINT ck_tblyearmaster_yearstatus
        CHECK (yearstatus IN ('Open', 'Closed', 'Planned'))
);

-- ============================================================
-- Table and column comments (visible in pg_catalog / psql \d+)
-- ============================================================

COMMENT ON TABLE fps.tblyearmaster
    IS 'Master table of fiscal / FPS years. '
       'Defines which years exist, their display codes, '
       'and their lifecycle status (Open, Closed, Planned).';

COMMENT ON COLUMN fps.tblyearmaster.fpsyear
    IS 'Four-digit calendar year that starts the fiscal period '
       '(e.g. 2025 for Apr 2025 – Mar 2026). Primary key.';

COMMENT ON COLUMN fps.tblyearmaster.fpsyearcode
    IS 'Human-readable fiscal-year label, e.g. FPS2025-26.';

COMMENT ON COLUMN fps.tblyearmaster.yearstatus
    IS 'Lifecycle state: Open (transactions allowed), '
       'Closed (read-only), or Planned (configuration only).';

COMMENT ON COLUMN fps.tblyearmaster.remarks
    IS 'Free-text note explaining the status or special conditions.';

COMMENT ON COLUMN fps.tblyearmaster.active
    IS 'Soft-delete flag. TRUE = visible to the application.';

COMMENT ON COLUMN fps.tblyearmaster.createdon
    IS 'Timestamp when the row was first inserted (auto-set).';

COMMENT ON COLUMN fps.tblyearmaster.createdby
    IS 'User or service account that created the row.';

-- ============================================================
-- SEED DATA
-- ============================================================
-- Sourced from reports/tblyearmaster.csv.
-- Three fiscal years representing each lifecycle state.
-- ============================================================

INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active)
VALUES
    (2024, '2024-25', 'Closed',  'Historical year, read-only',                  TRUE),
    (2025, '2025-26', 'Open',    'Active year, transactions allowed',            TRUE),
    (2026, '2026-27', 'Planned', 'Future year, planning/configuration only',     TRUE)
ON CONFLICT (fpsyear) DO NOTHING;
