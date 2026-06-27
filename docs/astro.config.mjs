import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import { unified } from '@astrojs/markdown-remark';
import starlightAutoDrafts from 'starlight-auto-drafts';
import starlightGithubAlerts from 'starlight-github-alerts';
import starlightSidebarTopics from 'starlight-sidebar-topics';
import starlightLinksValidator from 'starlight-links-validator';
import starlightHeadingBadges from 'starlight-heading-badges';
import starlightImageZoom from 'starlight-image-zoom';
import starlightScrollToTop from 'starlight-scroll-to-top';
import starlightPageActions from 'starlight-page-actions';
import { starlightIconsPlugin } from 'starlight-plugin-icons';
import starlightTags from 'starlight-tags';
import starlightChangelogs from 'starlight-changelogs';
import starlightLlmsTxt from 'starlight-llms-txt';

// In GitHub Actions, the site is served under /Extensions; locally we run at the root.
const base = process.env.GITHUB_ACTIONS ? '/Extensions' : '';

export default defineConfig({
    site: 'https://rocketsurgeonsguild.github.io',
    base,
    markdown: {
        processor: unified(),
    },
    integrations: [
        starlight({
            title: 'Extensions',
            description: 'Compile-time assembly/type-scanning for .NET. AOT-safe, zero runtime reflection.',
            social: [
                {
                    icon: 'github',
                    label: 'GitHub',
                    href: 'https://github.com/RocketSurgeonsGuild/Extensions',
                },
            ],
            plugins: [
                starlightAutoDrafts(),
                starlightGithubAlerts(),
                starlightSidebarTopics(
                    [
                        {
                            label: 'Getting Started',
                            link: '/guide/',
                            icon: 'open-book',
                            items: [{ autogenerate: { directory: 'guide' } }],
                        },
                        {
                            label: 'Reference',
                            link: '/reference/',
                            icon: 'information',
                            items: [{ autogenerate: { directory: 'reference' } }],
                        },
                        {
                            label: 'Architecture',
                            link: '/architecture/',
                            icon: 'puzzle',
                            items: [{ autogenerate: { directory: 'architecture' } }],
                        },
                        {
                            label: 'API Reference',
                            link: '/api/',
                            icon: 'seti:brackets',
                            items: [{ autogenerate: { directory: 'api' } }],
                        },
                        {
                            label: 'Changelog',
                            link: '/changelog/',
                            icon: 'list-format',
                        },
                    ],
                    {
                        exclude: ['/tags/**', '/changelog/**'],
                    }
                ),
                starlightLinksValidator({ errorOnRelativeLinks: false }),
                starlightHeadingBadges(),
                starlightImageZoom(),
                starlightScrollToTop(),
                starlightPageActions({
                    editLink: 'https://github.com/RocketSurgeonsGuild/Extensions/edit/main/docs/src/content/docs/',
                }),
                starlightIconsPlugin(),
                starlightTags(),
                starlightChangelogs(),
                starlightLlmsTxt(),
            ],
        }),
    ],
});
