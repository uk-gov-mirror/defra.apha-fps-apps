-- Table: fps.workgroupgrade

CREATE TABLE fps.workgroupgrade (
    wggrade citext NOT NULL,
    profitcentregrade citext NOT NULL,
    gradecode citext NOT NULL,
    workgroup citext NOT NULL,
    chargeratewg money,
    directratewg money DEFAULT 0,
    payratewg money DEFAULT 0,
    nprwg money DEFAULT 0,
    ohrwg money DEFAULT 0,
    avsalary money DEFAULT 0,
    hrschangedby character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_workgroupgrade PRIMARY KEY (wggrade, fpsyear),
    CONSTRAINT fk_workgroupgrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear),
    CONSTRAINT fk_workgroupgrade_profitcentregrade FOREIGN KEY (profitcentregrade, fpsyear) REFERENCES fps.profitcentregrade(pcgrade, fpsyear),
    CONSTRAINT fk_workgroupgrade_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear),

    CONSTRAINT fk_workgroupgrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
