-- View: fps.vpactprofitcentregrade

CREATE OR REPLACE VIEW fps.vpactprofitcentregrade AS
 SELECT pcgrade AS pc_grade,
    divisiongrade,
    gradecode,
    profitcentre,
    chargerate,
    directrate,
    payrate,
    npr,
    ohr,
    hrsavailable,
    fpsyear
   FROM fps.profitcentregrade;
