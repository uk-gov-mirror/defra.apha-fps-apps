-- Table: fps.monthlyoutput

CREATE TABLE fps.monthlyoutput (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    month double precision NOT NULL,
    workgroup citext NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_monthlyoutput PRIMARY KEY (testcode, buyer, month, workgroup, fpsyear),
    CONSTRAINT fk_monthlyoutput_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear),
    CONSTRAINT fk_monthlyoutput_testcode_workgroup FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability(testcode, workgroup, fpsyear),

    CONSTRAINT fk_monthlyoutput_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
