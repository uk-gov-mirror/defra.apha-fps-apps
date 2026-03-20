-- View: fps.vtblwgemployee

CREATE OR REPLACE VIEW fps.vtblwgemployee AS
SELECT
    e.pactid,
    e.spnumber,
    e.workgroupgrade,
    e.personstatus,
    e.personclass,
    e.hrspaid,
    e.leave,
    e.sickspecial,
    e.hrsavail,
    e.makeavailable,
    e.timerecorder,
    e.startdate,
    e.enddate,
    e.hoursperweek,
    e.fpsyear,
    wgg.user_id,
    wgg.dt2username,
    wgg.useremail
FROM fps.tblwgemployee e
JOIN fps.vworkgroupgrade wgg ON wgg.wggrade = e.workgroupgrade
                             AND wgg.fpsyear = e.fpsyear;
