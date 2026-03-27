# Apple Search Ads Attribution & Date Matching (ROAS Tracker)

## The two data sources

| Source | What you get | Date dimension |
|--------|----------------|----------------|
| **ASA API** (reports) | Aggregated metrics: taps, spend, installs (tapInstalls, totalNewDownloads, etc.) by campaign / ad group / keyword | Yes – use `startTime`, `endTime`, `granularity: "DAILY"`. Rows have a date (e.g. in `total.date` or `granularity`). |
| **AdServices postback** (tap-through payload) | One conversion per install/event; same IDs (campaignId, adGroupId, keywordId, etc.) | **Only when ATT allowed**: `clickDate` is present. Without ATT, payload has **no** click date. |

So: you can always filter **ASA report** data by date. You can only filter **postback**-derived conversions by (click) date when the payload is “detailed” (user allowed ATT and you get `clickDate`).

---

## Matching ASA report data with postback data

**Join keys** (from postback → report dimensions):

- `orgId` – matches your ASA org (you already have this on credentials).
- `campaignId` → campaign-level report / `metadata.campaignId`.
- `adGroupId` → ad group level (if you use ad group reports).
- `keywordId` → keyword-level report / `metadata.keywordId`.
- `countryOrRegion` → use `groupBy: ["countryOrRegion"]` if you want country-level ROAS.

**Flow:**

1. **Cost (spend)**  
   From ASA only. Request campaign (or keyword) reports with `startTime`, `endTime`, `granularity: "DAILY"`. Each row = one date (and optionally campaign/ad group/keyword/country). Use that as “cost by date” (and by dimension).

2. **Conversions / revenue**  
   From your backend: events you stored from AdServices postbacks (and optionally RevenueCat).  
   - When postback has **`clickDate`** (ATT): treat that as the **click date** for the conversion. Store it and use it for date filtering and for joining to ASA report rows (same date, same campaign/ad group/keyword).  
   - When postback has **no `clickDate`** (standard payload): you do **not** have a click date. You only have “conversion received” (and possibly install/purchase time from RevenueCat). You cannot truthfully assign that conversion to a specific **click** date.

3. **ROAS by date**  
   - **With ATT (detailed payload):**  
     - Conversions: filter stored postbacks by `clickDate` in the requested date range.  
     - Cost: use ASA report rows for the same date range (and same dimensions).  
     - ROAS = (revenue from postbacks in that date range) / (spend from ASA in that date range). You can do this at campaign, ad group, or keyword level if you store and group by those IDs.  
   - **Without ATT (standard payload):**  
     You cannot filter conversions by **click** date. Options:  
     - **Option A:** Use a **conversion date** (e.g. postback received_at or RevenueCat event time) as a proxy. Then “by date” means “by conversion date”, not “by click date” – ROAS will be approximate and may not align perfectly with ASA’s daily spend.  
     - **Option B:** Do not filter these conversions by date; show them in an “all time” or “unattributed (no click date)” bucket and only compute ROAS for the ATT segment by date.  
     - **Option C:** Mix: use conversion date for display/filtering but document that non-ATT conversions are “by conversion date”, not “by click date”.  
     - **Option D (recommended):** Use **first app open timestamp** as a proxy (see below).

---

## First app open timestamp (fallback when `clickDate` is missing)

When the postback has **no `clickDate`** (standard payload), use a **RevenueCat custom subscriber attribute** for the user’s **first app open timestamp**. It’s a better proxy for “when did this install/click likely happen” than:

- **Postback received_at** – depends on when your server got the callback.
- **Purchase / event timestamp** – can be days or weeks after install.

First open is usually same day or next day after install, so it’s closer to the real click date and still lets you filter and join by date for ROAS.

### Implementation

1. **In the app (iOS)**  
   On first app open (e.g. in your app delegate or root view), set a RevenueCat subscriber attribute once, e.g.:
   - Key: `$firstAppOpenTimestamp` (or a custom key like `first_app_open_timestamp`).
   - Value: ISO 8601 or Unix ms of first open.

   Use RevenueCat’s “set once” semantics if available so you don’t overwrite it on later launches.

2. **In the backend**  
   - When you receive an **AdServices postback without `clickDate`**, you have (or can resolve) an **app_user_id** that links to RevenueCat.  
   - Either:
     - **From webhooks:** When you process RevenueCat webhooks, `event.subscriber_attributes` can contain this attribute; store it with the user (or with the conversion record you link to the postback).  
     - **On demand:** When storing a postback without `clickDate`, resolve the user and read the first-app-open attribute from your own DB (if you sync RevenueCat attributes) or from RevenueCat’s API.  
   - Store this as something like **`attribution_date_proxy`** (nullable) on the conversion/postback record: use `clickDate` when present, otherwise first app open date (day-only or full timestamp, depending on how you join to ASA).

3. **ROAS by date**  
   - For conversions **with `clickDate`**: keep using `clickDate`.  
   - For conversions **without `clickDate`**: filter and join by **date of first app open** (e.g. truncate to day in the same timezone as your ASA reports).  
   - In the UI or API, you can label this as “by click date (ATT) or first open date (non-ATT)” so it’s clear it’s not 100% click date for the non-ATT segment.

### Caveat

Not every user opens the app the same day they install, so first open can be 1–2 days (or more) after the click. For ROAS this is usually acceptable: you’re still much closer than “conversion date” or “received at”, and you can still filter and compare by date ranges.

---

## Practical recommendations

1. **Persist every postback**  
   Store at least: `orgId`, `campaignId`, `adGroupId`, `keywordId`, `adId`, `conversionType`, `countryOrRegion`, `claimType`, and **`clickDate` when present**. Add `receivedAt` (server time) for all. Link to `app_user_id` so you can attach RevenueCat data (revenue + first app open). Optionally store **`attribution_date_proxy`** (e.g. first app open date) when `clickDate` is null.

2. **Date for filtering and ROAS**  
   - When **`clickDate`** is present (ATT): use it for filtering and for joining to ASA report rows.  
   - When **`clickDate`** is missing: use **first app open timestamp** (RevenueCat custom attribute) as `attribution_date_proxy` so you can still filter by date and compute ROAS; label as “first open date” in the UI.

3. **ASA reports**  
   - Keep using `startTime` / `endTime` and `granularity: "DAILY"` so every report row has a date.  
   - Use the same dimensions (campaign, ad group, keyword, country) as in your postback storage so you can join:  
     - Cost from ASA row (e.g. `row.total.localSpend`, `row.total.tapInstalls`)  
     - Conversions/revenue from your stored postbacks filtered by that date (and same campaign/ad group/keyword).

4. **ROAS endpoint**  
   - Input: date range, optional campaign/ad group/keyword.  
   - Cost: from ASA report API for that range (and dimensions).  
   - Revenue: from your DB – sum revenue for postbacks where **`clickDate` or `attribution_date_proxy`** (e.g. first app open date) falls in range, with dimensions matching. Prefer `clickDate` when present; use proxy for standard payloads.

---

## Summary

- **Matching:** Use `(orgId,) campaignId, adGroupId, keywordId` (and optionally `countryOrRegion`) from the postback to align with ASA report dimensions.  
- **Filter by date:**  
  - **Best:** Use **`clickDate`** when present (detailed, ATT-allowed payload).  
  - **Fallback when `clickDate` is missing:** Use **first app open timestamp** (RevenueCat custom attribute) as a proxy so you can still filter by date and compute ROAS; it’s closer to the real click than “received at” or purchase time, even though not 100% of users open the app the same day they install.

If you want, the next step is to add an **AdServices postback endpoint** and a **table** to store these payloads (with `clickDate` nullable), plus a small service to query them by date and dimensions for the ROAS calculation.
