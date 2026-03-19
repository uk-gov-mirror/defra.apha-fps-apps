-- Table: fps.tlkpprogram

CREATE TABLE fps.tlkpprogram (
    programno citext NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50) DEFAULT 'Charge'::character varying,
    customer character varying(50),
    target money DEFAULT 0,
    manager character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpprogram PRIMARY KEY (programno, fpsyear),

    CONSTRAINT fk_tlkpprogram_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
