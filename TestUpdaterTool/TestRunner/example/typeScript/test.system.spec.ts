import { test, expect } from '@playwright/test';

// Custom metadata interface for tests
interface TestMetadata {
    title: string;
    testCaseId: number;
}

test.describe('Example Playwright Test Suite with Metadata', () => {

    test('Test 1: Check homepage title', async ({ page }, testInfo) => {
        const metadata: TestMetadata = {
            title: 'Ensure homepage title is correct',
            testCaseId: 13,
        };
        testInfo.annotations.push(metadata);

        await test.step('Navigate to the homepage', async () => {
            await page.goto('https://example.com');
        });

        await test.step('Verify the page title', async () => {
            const title = await page.title();
            expect(title).toBe('Example Domain');
        });

        await test.step('Check for presence of heading', async () => {
            const heading = page.locator('h1');
            await expect(heading).toHaveText('Example Domain');
        });
    });

    test('Test 2: Perform a search operation', async ({ page }, testInfo) => {
        const metadata: TestMetadata = {
            title: 'Verify search operation on the site',
            testCaseId: 14,
        };
        testInfo.annotations.push(metadata);

        await test.step('Navigate to the search page', async () => {
            await page.goto('https://example.com');
        });

        await test.step('Click on the "More information..." link', async () => {
            const link = page.locator('a');
            await link.click();
        });

        await test.step('Verify URL changes after clicking the link', async () => {
            await expect(page).toHaveURL('https://www.iana.org/domains/example');
        });
    });

    test('Test 3: Validate form behavior', async ({ page }, testInfo) => {
        const metadata: TestMetadata = {
            title: 'Form validation test case',
            testCaseId: 15,
        };
        testInfo.annotations.push(metadata);

        await test.step('Navigate to the form page', async () => {
            await page.goto('https://example.com');
        });

        await test.step('Verify form input is visible', async () => {
            const input = page.locator('input[type="text"]');
            expect(await input.isVisible()).toBeTruthy();
        });

        await test.step('Fill the form and submit', async () => {
            const input = page.locator('input[type="text"]');
            await input.fill('Playwright Test');
            await input.press('Enter');
            // Add more assertions if required
        });
    });

});