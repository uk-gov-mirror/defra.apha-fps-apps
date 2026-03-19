-- Table: fps.tbltestrccost

CREATE TABLE fps.tbltestrccost (
    testcode citext NOT NULL,
    profitcentre citext NOT NULL,
    price money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestrccost PRIMARY KEY (testcode, profitcentre, fpsyear),
    CONSTRAINT fk_tbltestrccost_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre),
    CONSTRAINT fk_tbltestrccost_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear),

    CONSTRAINT fk_tbltestrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
