-- Table: fps.projectmonth

CREATE TABLE fps.projectmonth (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    costprofile money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_projectmonth PRIMARY KEY (project, monthno, fpsyear),

    CONSTRAINT fk_projectmonth_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
