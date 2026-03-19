-- Table: fps.workgroupmonth

CREATE TABLE fps.workgroupmonth (
    workgroup character varying(50) NOT NULL,
    month double precision NOT NULL,
    runningcost money,
    runcostprofile money,
    fpsyear integer NOT NULL,

    CONSTRAINT pk_workgroupmonth PRIMARY KEY (workgroup, month, fpsyear),

    CONSTRAINT fk_workgroupmonth_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
