-- View: fps.pandoratimecostcalcs

CREATE OR REPLACE VIEW fps.pandoratimecostcalcs AS
SELECT DISTINCT
    tc.workgroup,
    tc.jobcode,
    tc.project,
    tc.month,
    tc.staffid,
    tc.gradecode,
    tc.name,
    tc.chargerate,
    tc.class,
    tc."time",
    tc.cost,
    tc.division,
    tc.jobcodeold,
    tc.pay,
    tc.nonpay,
    tc.overhead,
    tc.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.timecostcalcs tc
JOIN fps.tbluser_workgroup uw ON tc.workgroup = uw.workgroup
JOIN fps.tblusers u           ON uw.user_id   = u.user_id;
