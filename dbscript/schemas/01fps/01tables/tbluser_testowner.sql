-- Table: fps.tbluser_testowner

CREATE TABLE fps.tbluser_testowner (
    user_id integer NOT NULL,
    test_owner character varying(2) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_testowner PRIMARY KEY (test_owner, user_id, fpsyear),

    CONSTRAINT fk_tbluser_testowner_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
