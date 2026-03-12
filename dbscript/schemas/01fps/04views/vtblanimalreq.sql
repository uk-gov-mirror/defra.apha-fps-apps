-- View: fps.vtblanimalreq

CREATE OR REPLACE VIEW fps.vtblanimalreq AS
 SELECT ar.jobcode,
    ar.animaltype,
    ar.numberofdays,
    ar.numberofanimals,
    ar.indcounter,
    ar.fpsyear
   FROM fps.tblanimalreq ar
   JOIN fps.vtlkpproject p ON p.parentproject = ar.jobcode AND p.fpsyear = ar.fpsyear;
