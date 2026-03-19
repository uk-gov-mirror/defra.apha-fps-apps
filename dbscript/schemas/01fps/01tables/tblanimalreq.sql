-- Table: fps.tblanimalreq

CREATE TABLE fps.tblanimalreq (
    jobcode citext NOT NULL,
    animaltype citext NOT NULL,
    numberofdays double precision DEFAULT 0 NOT NULL,
    numberofanimals double precision DEFAULT 0 NOT NULL,
    indcounter integer DEFAULT nextval('fps.tblanimalreq_indcounter_seq'::regclass) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblanimalreq PRIMARY KEY (indcounter, fpsyear),
    CONSTRAINT fk_tblanimalreq_animaltype FOREIGN KEY (animaltype, fpsyear) REFERENCES fps.tblanimals(animaltype, fpsyear),
    CONSTRAINT fk_tblanimalreq_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_tblanimalreq_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
