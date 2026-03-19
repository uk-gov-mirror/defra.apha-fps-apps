-- Table: fps.tbltestreqwg

CREATE TABLE fps.tbltestreqwg (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    workgroup character varying(50) NOT NULL,
    amount integer DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestreqwg PRIMARY KEY (testcode, buyer, workgroup, fpsyear),

    CONSTRAINT fk_tbltestreqwg_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
