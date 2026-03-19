-- Table: fps.grade

CREATE TABLE fps.grade (
    gradecode citext NOT NULL,
    desc_long character varying(30),
    avsalary money DEFAULT 0,
    pactcode character varying(50),
    avleavehrs double precision DEFAULT 0,
    avsickhrs double precision DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_grade PRIMARY KEY (gradecode, fpsyear),

    CONSTRAINT fk_grade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
