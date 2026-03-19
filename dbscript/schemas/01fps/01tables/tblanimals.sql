-- Table: fps.tblanimals

CREATE TABLE fps.tblanimals (
    animaltype citext NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean DEFAULT false NOT NULL,
    defradailyrate money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblanimals PRIMARY KEY (animaltype, fpsyear),

    CONSTRAINT fk_tblanimals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
