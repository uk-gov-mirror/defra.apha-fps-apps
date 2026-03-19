-- Table: fps.tbltestreqbaseline

CREATE TABLE fps.tbltestreqbaseline (
    program character varying(10) NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    norequired integer,
    unitprice money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestreqbaseline PRIMARY KEY (program, testcode, buyer, fpsyear),

    CONSTRAINT fk_tbltestreqbaseline_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
