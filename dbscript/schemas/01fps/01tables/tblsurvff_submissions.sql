-- Table: fps.tblsurvff_submissions

CREATE TABLE fps.tblsurvff_submissions (
    sd_pact_wg character varying(50) NOT NULL,
    contract character varying(20) NOT NULL,
    countofjobname integer,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblsurvff_submissions PRIMARY KEY (sd_pact_wg, contract, fpsyear),

    CONSTRAINT fk_tblsurvff_submissions_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
