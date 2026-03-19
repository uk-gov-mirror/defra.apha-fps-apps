-- View: fps.qryjobmonth_transferunion

CREATE OR REPLACE VIEW fps.qryjobmonth_transferunion AS
 SELECT qryjobmonth_tctransfers.project,
    qryjobmonth_tctransfers.month,
    qryjobmonth_tctransfers.fpsyear,
    qryjobmonth_tctransfers.transfercost
   FROM fps.qryjobmonth_tctransfers
UNION ALL
 SELECT qryjobmonth_transfers1.project,
    qryjobmonth_transfers1.month,
    qryjobmonth_transfers1.fpsyear,
    qryjobmonth_transfers1.transfercost
   FROM fps.qryjobmonth_transfers1;
