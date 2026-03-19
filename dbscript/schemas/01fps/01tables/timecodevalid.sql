-- Table: fps.timecodevalid

CREATE TABLE fps.timecodevalid (
    timecode citext NOT NULL,
    workgroup citext NOT NULL,
    parentproject citext NOT NULL,
    testcode character varying(50),
    jobcode character varying(50),
    portfolio character varying(20),
    active boolean NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_timecodevalid PRIMARY KEY (workgroup, timecode, parentproject, fpsyear),
    CONSTRAINT fk_timecodevalid_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_timecodevalid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
