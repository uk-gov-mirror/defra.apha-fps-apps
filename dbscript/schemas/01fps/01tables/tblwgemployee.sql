-- Table: fps.tblwgemployee

CREATE TABLE fps.tblwgemployee (
    pactid citext NOT NULL,
    spnumber citext NOT NULL,
    workgroupgrade citext NOT NULL,
    personstatus character varying(10) DEFAULT 'A'::character varying NOT NULL,
    personclass character varying(10),
    hrspaid double precision NOT NULL,
    leave double precision NOT NULL,
    sickspecial double precision NOT NULL,
    hrsavail double precision NOT NULL,
    makeavailable integer DEFAULT '-1'::integer NOT NULL,
    timerecorder integer DEFAULT 0 NOT NULL,
    startdate date,
    enddate date,
    hoursperweek double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblwgemployee PRIMARY KEY (pactid, fpsyear),
    CONSTRAINT fk_tblwgemployee_spnumber FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee(spnumber, fpsyear),

    CONSTRAINT fk_tblwgemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear),

    CONSTRAINT fk_tblwgemployee_workgroupgrade FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade(wggrade, fpsyear)
);
