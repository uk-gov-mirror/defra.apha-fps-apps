-- View: fps.vanimalcosts

CREATE OR REPLACE VIEW fps.vanimalcosts AS
 SELECT tblanimalreq.numberofdays,
    tblanimalreq.numberofanimals,
    tblanimalreq.fpsyear
   FROM (fps.tblanimals
     JOIN fps.tblanimalreq ON ((((tblanimals.animaltype)::text = (tblanimalreq.animaltype)::text) AND (tblanimals.fpsyear = tblanimalreq.fpsyear))));
