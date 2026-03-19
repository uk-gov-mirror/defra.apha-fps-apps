-- Table: fps.tbladminusers

CREATE TABLE fps.tbladminusers (
    mnumber character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    seedeptincome boolean DEFAULT false NOT NULL,
    seedbwindow boolean DEFAULT false NOT NULL,
    dt2number character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbladminusers PRIMARY KEY (mnumber, fpsyear),

    CONSTRAINT fk_tbladminusers_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
