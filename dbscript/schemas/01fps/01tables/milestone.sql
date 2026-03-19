-- Table: fps.milestone

CREATE TABLE fps.milestone (
    project citext NOT NULL,
    milestoneref character varying(4) NOT NULL,
    objectiveref character varying(50) NOT NULL,
    milsetonetitle character varying(120),
    plandate date,
    actualdate date,
    comment text,
    monthnofin double precision,
    year character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_milestone PRIMARY KEY (project, milestoneref, objectiveref, fpsyear),
    CONSTRAINT fk_milestone_project FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_milestone_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
