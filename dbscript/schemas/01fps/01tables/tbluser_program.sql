-- Table: fps.tbluser_program

CREATE TABLE fps.tbluser_program (
    user_id integer NOT NULL,
    programno character varying(10) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_program PRIMARY KEY (programno, user_id, fpsyear),

    CONSTRAINT fk_tbluser_program_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
