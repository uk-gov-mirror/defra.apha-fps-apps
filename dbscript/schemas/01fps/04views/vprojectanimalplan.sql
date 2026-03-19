-- View: fps.vprojectanimalplan

CREATE OR REPLACE VIEW fps.vprojectanimalplan AS
 SELECT tlkpproject.parentproject,
    tlkpproject.program,
    tblanimalreq.animaltype,
    tblanimalreq.numberofdays,
    tblanimalreq.numberofanimals,
        CASE tlkpproject.isdefraproject
            WHEN 0 THEN tblanimals.dailyrate
            ELSE tblanimals.defradailyrate
        END AS dailyrate,
    ((tblanimalreq.numberofanimals * tblanimalreq.numberofdays) *
        CASE tlkpproject.isdefraproject
            WHEN 0 THEN tblanimals.dailyrate
            ELSE tblanimals.defradailyrate
        END) AS cost,
    tblanimals.species,
    tblanimals.security_level,
    tblanimalreq.indcounter,
    tblanimalreq.fpsyear
   FROM ((fps.tlkpproject
     JOIN fps.tblanimalreq ON ((((tlkpproject.parentproject)::text = (tblanimalreq.jobcode)::text) AND (tlkpproject.fpsyear = tblanimalreq.fpsyear))))
     JOIN fps.tblanimals ON ((((tblanimalreq.animaltype)::text = (tblanimals.animaltype)::text) AND (tblanimalreq.fpsyear = tblanimals.fpsyear))));
