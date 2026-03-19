-- Table: fps.profitcentregrade_nondefra

CREATE TABLE fps.profitcentregrade_nondefra (
    pcgrade character varying(20) NOT NULL,
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    profitcentre citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_profitcentregrade_nondefra PRIMARY KEY (pcgrade, fpsyear),
    CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear),
    CONSTRAINT fk_profitcentregrade_nondefra_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear),
    CONSTRAINT fk_profitcentregrade_nondefra_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre),

    CONSTRAINT fk_profitcentregrade_nondefra_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
