-- Table: fps.tbltestrequirementrccost

CREATE TABLE fps.tbltestrequirementrccost (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    profitcentre citext NOT NULL,
    price money NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestrequirementrccost PRIMARY KEY (testcode, buyer, profitcentre, fpsyear),
    CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear),
    CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost(testcode, profitcentre, fpsyear),

    CONSTRAINT fk_tbltestrequirementrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
