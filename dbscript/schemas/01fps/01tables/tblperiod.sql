-- Table: fps.tblperiod

CREATE TABLE fps.tblperiod (
    periodname character varying(50) NOT NULL,
    periodtype character varying(50),
    startperiod double precision,
    endperiod double precision,
    finalsummariesrun smallint,
    periodlocked smallint DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblperiod PRIMARY KEY (periodname, fpsyear),

    CONSTRAINT fk_tblperiod_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
