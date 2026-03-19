-- Table: fps.divisiongrade

CREATE TABLE fps.divisiongrade (
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    division citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_divisiongrade PRIMARY KEY (divisiongrade, fpsyear),
    CONSTRAINT fk_divisiongrade_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname),
    CONSTRAINT fk_divisiongrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear),

    CONSTRAINT fk_divisiongrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
