-- Table: fps.tblpaymentschedule

CREATE TABLE fps.tblpaymentschedule (
    contract citext NOT NULL,
    duedate timestamp without time zone NOT NULL,
    paid smallint NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblpaymentschedule PRIMARY KEY (contract, duedate, fpsyear),
    CONSTRAINT fk_tblpaymentschedule_contract FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract(contractno, fpsyear),

    CONSTRAINT fk_tblpaymentschedule_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
