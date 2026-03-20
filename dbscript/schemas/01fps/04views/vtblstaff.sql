-- View: fps.vtblstaff

CREATE OR REPLACE VIEW fps.vtblstaff AS
SELECT
    tblwgemployee.pactid AS staffid,
    (COALESCE(tblemployee.lastname, '')::text || ', ' || COALESCE(tblemployee.firstname, '')::text) AS name,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.leave,
    tblwgemployee.sickspecial,
    tblwgemployee.hrsavail,
    tblwgemployee.makeavailable,
    tblwgemployee.fpsyear,
    wgg.user_id,
    wgg.dt2username,
    wgg.useremail
FROM fps.tblwgemployee
JOIN fps.tblemployee ON tblemployee.spnumber = tblwgemployee.spnumber
JOIN fps.vworkgroupgrade wgg ON wgg.wggrade = tblwgemployee.workgroupgrade
                             AND wgg.fpsyear = tblwgemployee.fpsyear;
