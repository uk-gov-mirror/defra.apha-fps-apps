-- Table: fps.tlkptestcapability

CREATE TABLE fps.tlkptestcapability (
    testcode citext NOT NULL,
    workgroup citext NOT NULL,
    planportfolio citext NOT NULL,
    unitcost money DEFAULT 0,
    predoutturn double precision DEFAULT 0,
    sop character varying(50),
    smscode character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkptestcapability PRIMARY KEY (testcode, workgroup, fpsyear),
    CONSTRAINT fk_tlkptestcapability_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear),

    CONSTRAINT fk_tlkptestcapability_planportfolio FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_tlkptestcapability_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear),

    CONSTRAINT fk_tlkptestcapability_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear)
);
