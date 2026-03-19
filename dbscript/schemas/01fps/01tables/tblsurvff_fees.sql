-- Table: fps.tblsurvff_fees

CREATE TABLE fps.tblsurvff_fees (
    pactcode character varying(50) NOT NULL,
    owning_vic character varying(50) NOT NULL,
    received timestamp without time zone,
    contract character varying(20) NOT NULL,
    record_id character varying(20) NOT NULL,
    volume double precision,
    totalfee money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblsurvff_fees PRIMARY KEY (owning_vic, contract, record_id, fpsyear),

    CONSTRAINT fk_tblsurvff_fees_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
