-- Table: fps.tblstaffjob

CREATE TABLE fps.tblstaffjob (
    staffid citext NOT NULL,
    jobcode citext NOT NULL,
    plannedhours double precision DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblstaffjob PRIMARY KEY (staffid, jobcode, fpsyear),
    CONSTRAINT fk_tblstaffjob_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_tblstaffjob_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear),

    CONSTRAINT fk_tblstaffjob_staffid FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear)
);
