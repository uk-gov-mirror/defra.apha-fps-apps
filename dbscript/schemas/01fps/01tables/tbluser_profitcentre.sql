-- Table: fps.tbluser_profitcentre

CREATE TABLE fps.tbluser_profitcentre (
    profitcentre character varying(50) NOT NULL,
    user_id integer NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_profitcentre PRIMARY KEY (profitcentre, user_id, fpsyear),

    CONSTRAINT fk_tbluser_profitcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
