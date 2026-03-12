-- View: fps.vpostmortem1report_obsolete

CREATE OR REPLACE VIEW fps.vpostmortem1report_obsolete AS
SELECT testcode,
    itemdescription,
    fpsyear,
    totvol,
    ltunitcharge,
    sdunitcharge,
    (round((ltfee)::numeric, 0))::integer AS ltfee,
    (round((sdfee)::numeric, 0))::integer AS sdfee,
    ((round((ltfee)::numeric, 0))::integer + (round((sdfee)::numeric, 0))::integer) AS "total fee",
    (round((feecharged)::numeric, 0))::integer AS "fee charged",
    (round((((feecharged)::numeric - (ltfee)::numeric) - (sdfee)::numeric), 0))::integer AS "profit/loss",
    workgroup
   FROM fps.vpostmort1;
