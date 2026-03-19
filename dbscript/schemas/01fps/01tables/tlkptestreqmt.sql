-- Table: fps.tlkptestreqmt

CREATE TABLE fps.tlkptestreqmt (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    active smallint DEFAULT 1,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkptestreqmt PRIMARY KEY (testcode, buyer, fpsyear),
    CONSTRAINT fk_tlkptestreqmt_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear),

    CONSTRAINT fk_tlkptestreqmt_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
