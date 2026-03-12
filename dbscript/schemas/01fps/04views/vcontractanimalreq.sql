-- View: fps.vcontractanimalreq

CREATE OR REPLACE VIEW fps.vcontractanimalreq AS
 SELECT ar.jobcode,
    ar.animaltype,
    ar.numberofdays,
    ar.numberofanimals,
    ar.indcounter,
    ar.fpsyear
   FROM fps.tblanimalreq ar
   JOIN fps.vcontractproject p ON p.parentproject = ar.jobcode AND p.fpsyear = ar.fpsyear;
