-- Table: fps.proj_subcontract

CREATE TABLE fps.proj_subcontract (
    subcontcounter integer DEFAULT nextval('fps.proj_subcontract_subcontcounter_seq'::regclass) NOT NULL,
    project citext,
    testjob character varying(50),
    month double precision,
    amount money,
    workgroup character varying(50),
    acctcode character varying(30),
    supplier character varying(50),
    description character varying(255),
    suppliernumber integer,
    dailyrate money,
    animaldays integer,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_proj_subcontract PRIMARY KEY (subcontcounter, fpsyear),
    CONSTRAINT fk_proj_subcontract_project FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_proj_subcontract_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
