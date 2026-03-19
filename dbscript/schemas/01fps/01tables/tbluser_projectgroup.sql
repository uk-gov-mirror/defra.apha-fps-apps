-- Table: fps.tbluser_projectgroup

CREATE TABLE fps.tbluser_projectgroup (
    user_id integer NOT NULL,
    projectgroup character varying(50) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_projectgroup PRIMARY KEY (projectgroup, user_id, fpsyear),

    CONSTRAINT fk_tbluser_projectgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
