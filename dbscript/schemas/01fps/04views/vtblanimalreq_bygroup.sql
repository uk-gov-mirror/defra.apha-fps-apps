-- View: fps.vtblanimalreq_bygroup

CREATE OR REPLACE VIEW fps.vtblanimalreq_bygroup AS
SELECT
    ar.jobcode,
    ar.animaltype,
    ar.numberofdays,
    ar.numberofanimals,
    ar.indcounter,
    ar.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.tblanimalreq ar
JOIN fps.vtlkpproject_bygroup p ON p.parentproject = ar.jobcode
                                AND p.fpsyear      = ar.fpsyear;
