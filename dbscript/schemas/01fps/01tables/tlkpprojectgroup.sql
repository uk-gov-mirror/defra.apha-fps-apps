-- Table: fps.tlkpprojectgroup

CREATE TABLE fps.tlkpprojectgroup (
    projectgroup citext NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpprojectgroup PRIMARY KEY (projectgroup, fpsyear),

    CONSTRAINT fk_tlkpprojectgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
