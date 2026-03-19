-- Table: fps.tbluser_category

CREATE TABLE fps.tbluser_category (
    user_id integer NOT NULL,
    category character varying(20) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_category PRIMARY KEY (user_id, category, fpsyear),

    CONSTRAINT fk_tbluser_category_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
