-- Table: fps.costcentre

CREATE TABLE fps.costcentre (
    costcentre double precision NOT NULL,
    profitcentre citext NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_costcentre PRIMARY KEY (costcentre, fpsyear),
    CONSTRAINT fk_costcentre_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre),

    CONSTRAINT fk_costcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
