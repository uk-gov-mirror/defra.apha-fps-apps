-- Table: fps.monthlytime

CREATE TABLE fps.monthlytime (
    pactstaffid citext NOT NULL,
    timecode citext NOT NULL,
    month double precision NOT NULL,
    parentproject citext NOT NULL,
    workgroup citext,
    hours double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_monthlytime PRIMARY KEY (pactstaffid, timecode, month, parentproject, fpsyear),
    CONSTRAINT fk_monthlytime_pactstaffid FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear),

    CONSTRAINT fk_monthlytime_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear),

    CONSTRAINT fk_monthlytime_timecodevalid FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid(workgroup, timecode, parentproject, fpsyear)
);
