-- Table: fps.tbltotalbusinessoverheads

CREATE TABLE fps.tbltotalbusinessoverheads (
    totalbusinessoverheads money,
    fpsyear integer NOT NULL,
    CONSTRAINT tb_pk UNIQUE (totalbusinessoverheads),

    CONSTRAINT pk_tbltotalbusinessoverheads PRIMARY KEY (fpsyear),

    CONSTRAINT fk_tbltotalbusinessoverheads_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
