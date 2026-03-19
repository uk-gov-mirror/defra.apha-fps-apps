-- Table: fps.profitcentregrade

CREATE TABLE fps.profitcentregrade (
    pcgrade citext NOT NULL,
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    profitcentre citext NOT NULL,
    chargerate money,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    defrachargerate money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_profitcentregrade PRIMARY KEY (pcgrade, fpsyear),
    CONSTRAINT fk_profitcentregrade_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear),
    CONSTRAINT fk_profitcentregrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear),
    CONSTRAINT fk_profitcentregrade_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre),

    CONSTRAINT fk_profitcentregrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
