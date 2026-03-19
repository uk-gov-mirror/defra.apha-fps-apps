-- Table: fps.tblemployee

CREATE TABLE fps.tblemployee (
    spnumber citext NOT NULL,
    firstname character varying(20),
    lastname character varying(20),
    title character varying(4),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblemployee PRIMARY KEY (spnumber, fpsyear),

    CONSTRAINT fk_tblemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
