-- Table: fps.tblpurchase

CREATE TABLE fps.tblpurchase (
    workgroup citext NOT NULL,
    account citext NOT NULL,
    itemdescription character varying(50) NOT NULL,
    amount money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblpurchase PRIMARY KEY (workgroup, account, itemdescription, fpsyear),
    CONSTRAINT fk_tblpurchase_workgroup_account FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid(workgroup, account, fpsyear),

    CONSTRAINT fk_tblpurchase_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
